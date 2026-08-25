using Dao.Interface;
using Entities;
using Entities.Dtos;
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

        public async Task<DataResponse<Driver>> GetAllDriversSession(int meetingKey, int sessionKey)
        {
            try
            {
                if (meetingKey <= 0) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Meeting key must be informed!");
                if (sessionKey <= 0) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>("Session key must be informed!");

                return await _unityOfWork.DriverDao.GetAllDriversSession(meetingKey, sessionKey);
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
                DataResponse<Driver> driversDatabase = await GetAllDriversSession(driverInsertDTO.MeetingKey, driverInsertDTO.SessionKey);

                if (!driversDatabase.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to search drivers in database: " + driversDatabase.Message);

                if (driversDatabase.Itens.Count == 22)
                    return ResponseFactory.CreateInstance().CreateSuccessResponse("All drivers already in database.");

                DataResponse<Driver> drivers = await _driverClient.GetAllDriversSessionSelected(driverInsertDTO);

                if (drivers.Itens == null || drivers.Itens.Count <= 0)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("The Drivers in the most recent meeting was not found");

                var newDrivers = drivers.Itens;
                if (driversDatabase.Itens.Count < 22)
                {
                    newDrivers = drivers.Itens
                        .Where(x => !driversDatabase.Itens.Any(y => y.DriverNumber == x.DriverNumber && y.SessionKey == x.SessionKey))
                        .ToList();
                }

                if (newDrivers == null || newDrivers.Count == 0)
                    return ResponseFactory.CreateInstance().CreateSuccessResponse("All drivers already in database.");

                Response response = await _unityOfWork.DriverDao.InsertDrivers(newDrivers);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert the driver: " + response.Message, response.Exception);

                response = await _unityOfWork.Commit();

                if (!response.HasSuccess) return response;

                return ResponseFactory.CreateInstance().CreateSuccessResponse("The new Driver has been insert!");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
