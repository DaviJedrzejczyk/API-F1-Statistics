using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class DriverService : IDriverService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IDriverClient _driverClient;

        public DriverService(IUnityOfWork unityOfWork, IDriverClient driverClient)
        {
            _unityOfWork = unityOfWork;
            _driverClient = driverClient;
        }

        public async Task<Response> DeleteDriver(Driver driver)
        {
            try
            {
                if (driver == null)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Driver must be informed.");

                Response response = await _unityOfWork.DriverDao.DeleteDriver(driver);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse(response.Message, response.Exception);

                return response;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey)
            => await GetAllDriversSession(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                if (sessionKey <= 0) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Session key must be informed!");

                return await SearchDriversExternalApi(new DriverInsertDTO { SessionKey = sessionKey }, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(ex);
            }
        }

        public async Task<SingleResponse<Driver>> GetDriverById(int id)
        {
            try
            {
                if (id <= 0)
                   return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Driver>("Id must be greater than 0");

                SingleResponse<Driver> response = await _unityOfWork.DriverDao.GetDriverById(id);

                if (response.Item == null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Driver>("Driver not found!");

                if (response.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Driver>("A error has ocurred while search the driver in database: " + response.Message, response.Exception);

                return response;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Driver>(ex);    
            }
        }

        public async Task<Response> InsertDrivers(List<Driver> drivers)
        {
            try
            {
                var rf = ResponseFactory.CreateInstance();

                if (drivers == null || drivers.Count == 0)
                    return rf.CreateFailureResponse("Driver list must be provided.");

                Response insertResponse = await _unityOfWork.DriverDao.InsertDrivers(drivers);
                if (!insertResponse.HasSuccess)
                    return rf.CreateFailureResponse("Failed to insert the driver(s): " + insertResponse.Message, insertResponse.Exception);

                Response commitResponse = await _unityOfWork.Commit();
                if (!commitResponse.HasSuccess)
                    return rf.CreateFailureResponse("Failed to commit the transaction: " + commitResponse.Message, commitResponse.Exception);

                return rf.CreateSuccessResponse("The new driver(s) have been inserted!");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<DataResponse<Driver>> SearchDriversExternalApi(DriverInsertDTO driverInsertDTO)
            => await SearchDriversExternalApi(driverInsertDTO, CancellationToken.None);

        private async Task<DataResponse<Driver>> SearchDriversExternalApi(DriverInsertDTO driverInsertDTO, CancellationToken cancellationToken)
        {
            string key = driverInsertDTO.SessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var cached = await ImportWindowCache.Read<Driver>(_unityOfWork, "drivers", key, cancellationToken);
            if (cached != null)
                return WindowResult(cached);

            DataResponse<Driver> drivers = await _driverClient.GetAllDriversSessionSelected(driverInsertDTO, cancellationToken);
            
            if (!drivers.HasSuccess || drivers.Itens == null)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(drivers.Message, drivers.Exception);

            if (drivers.Itens.Any(x => x.SessionKey != driverInsertDTO.SessionKey) ||
                drivers.Itens.Select(x => x.DriverNumber).Distinct().Count() != drivers.Itens.Count)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Invalid driver window returned by upstream.");

            await using var transaction = await _unityOfWork.BeginImport("drivers", key, cancellationToken);
            cached = await ImportWindowCache.Read<Driver>(_unityOfWork, "drivers", key, cancellationToken);
            if (cached != null)
                return WindowResult(cached);

            DataResponse<Driver> driversDatabase = await _unityOfWork.DriverDao.GetAllDriversSession(driverInsertDTO.SessionKey, cancellationToken);
            if (!driversDatabase.HasSuccess)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(driversDatabase.Message, driversDatabase.Exception);

            List<Driver> newDrivers = GetNewDrivers(driversDatabase, drivers);
            if (newDrivers.Count > 0)
            {
                var responseInsert = await _unityOfWork.DriverDao.InsertDrivers(newDrivers, cancellationToken);
                if (!responseInsert.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(responseInsert.Message, responseInsert.Exception);
            }
            cancellationToken.ThrowIfCancellationRequested();
            await ImportWindowCache.Stage(_unityOfWork, "drivers", key, drivers.Itens, cancellationToken);
            var commit = await _unityOfWork.Commit(cancellationToken);
            if (!commit.HasSuccess)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(commit.Message, commit.Exception);
            await transaction.CommitAsync(cancellationToken);
            return WindowResult(drivers.Itens);
        }

        private static DataResponse<Driver> WindowResult(List<Driver> drivers)
            => drivers.Count == 0
                ? ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("The Drivers in the most recent meeting was not found")
                : ResponseFactory.CreateInstance().CreateSuccessDataResponse(drivers);

        private static List<Driver> GetNewDrivers(DataResponse<Driver> driversDatabase, DataResponse<Driver> drivers)
        {
            var newDrivers = drivers.Itens
                    .Where(x => !driversDatabase.Itens.Any(y => y.DriverNumber == x.DriverNumber && y.SessionKey == x.SessionKey))
                    .ToList();

            return newDrivers;
        }

        public async Task<DataResponse<Driver>> SearchDriversDatabase(DriverInsertDTO driverInsertDTO)
        {
            DataResponse<Driver> driversDatabase = await _unityOfWork.DriverDao.GetAllDriversSession(driverInsertDTO.SessionKey);
            if (!driversDatabase.HasSuccess)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Failed to search drivers in database: " + driversDatabase.Message);

            return driversDatabase;
        }

        public async Task<SingleResponse<string>> GetDriverName(int driverNumber, int sessionKey)
        {
            try
            {
                var response = await _unityOfWork.DriverDao.GetDriverName(driverNumber, sessionKey);
                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Failed to get driver name: " + response.Message, response.Exception);
                
                if (string.IsNullOrEmpty(response.Item))
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Driver name not found for the given driver number and session key.");
                
                return response;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>(ex);
            }
        }
    }
}
