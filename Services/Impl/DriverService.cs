using Dao.Interface;
using Entities;
using Entities.Dtos.DriverDTOs;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
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
        {
            try
            {
                if (sessionKey <= 0) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Session key must be informed!");

                return await _unityOfWork.DriverDao.GetAllDriversSession(sessionKey);
            }
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

        public async Task<Response> InsertDrivers(DriverInsertDTO driverInsertDTO)
        {
            try
            {
                var rf = ResponseFactory.CreateInstance();

                if (driverInsertDTO == null)
                    return rf.CreateFailureResponse("DriverInsertDTO must be provided.");

                DataResponse<Driver> driversDatabase = await SearchDriversDatabase(driverInsertDTO);
                if (!driversDatabase.HasSuccess)
                    return rf.CreateFailureResponse(driversDatabase.Message, driversDatabase.Exception);

                if (driversDatabase.Itens == null || driversDatabase.Itens.Count == 22)
                    return rf.CreateSuccessResponse("All drivers already in database.");

                DataResponse<Driver> drivers = await SearchDriversExternalApi(driverInsertDTO);
                if (!drivers.HasSuccess)
                    return rf.CreateFailureResponse(drivers.Message, drivers.Exception);

                List<Driver> newDrivers = GetNewDrivers(driversDatabase, drivers);
                if (newDrivers == null || newDrivers.Count == 0)
                    return rf.CreateSuccessResponse("All drivers already in database.");

                Response insertResponse = await _unityOfWork.DriverDao.InsertDrivers(newDrivers);
                if (!insertResponse.HasSuccess)
                    return rf.CreateFailureResponse("Failed to insert the driver(s): " + insertResponse.Message, insertResponse.Exception);

                Response commitResponse = await _unityOfWork.Commit();
                if (!commitResponse.HasSuccess)
                    return commitResponse;

                return rf.CreateSuccessResponse("The new driver(s) have been inserted!");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        private static List<Driver> GetNewDrivers(DataResponse<Driver> driversDatabase, DataResponse<Driver> drivers)
        {
            var newDrivers = drivers.Itens;
            if (driversDatabase.Itens.Count < 22)
            {
                newDrivers = drivers.Itens
                    .Where(x => !driversDatabase.Itens.Any(y => y.DriverNumber == x.DriverNumber && y.SessionKey == x.SessionKey))
                    .ToList();
            }

            return newDrivers;
        }

        private async Task<DataResponse<Driver>> SearchDriversExternalApi(DriverInsertDTO driverInsertDTO)
        {
            DataResponse<Driver> drivers = await _driverClient.GetAllDriversSessionSelected(driverInsertDTO);
            
            if (drivers.Itens == null || drivers.Itens.Count <= 0)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("The Drivers in the most recent meeting was not found");
            
            return drivers;
        }

        private async Task<DataResponse<Driver>> SearchDriversDatabase(DriverInsertDTO driverInsertDTO)
        {
            DataResponse<Driver> driversDatabase = await GetAllDriversSession(driverInsertDTO.SessionKey);
            if (!driversDatabase.HasSuccess)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Failed to search drivers in database: " + driversDatabase.Message);

            if (driversDatabase.Itens.Count == 22)
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Driver>("All drivers already in database.");
            
            return driversDatabase;
        }
    }
}
