using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class DriverService : IDriverService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IDriverClient _driverClient;
        private readonly IMeetingService _meetingService;

        public DriverService(IUnityOfWork unityOfWork, IDriverClient driverClient, IMeetingService meetingService)
        {
            _unityOfWork = unityOfWork;
            _driverClient = driverClient;
            _meetingService = meetingService;
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

        public async Task<Response> InsertDrivers()
        {
            try
            {
                SingleResponse<int> meetingKey = await _meetingService.GetRecentMeetingKey();
                
                if (!meetingKey.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse(meetingKey.Message, meetingKey.Exception);


                DataResponse<Driver> drivers = await _driverClient.GetAllDriversRecentMeeting(meetingKey.Item);

                if (drivers.Itens == null || drivers.Itens.Count <= 0)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("The Drivers in the most recent meeting was not found");

                Response response = await _unityOfWork.DriverDao.InsertDrivers(drivers.Itens);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert the driver: " + response.Message, response.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessResponse("The new Driver has been insert!");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<Response> UpdateDriver(Driver driver)
        {
            try
            {
                if (driver == null)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Driver must be informed!");

                Response response = await _unityOfWork.DriverDao.UpdateDriver(driver);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse(response.Message, response.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessResponse("Driver has been updated!");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
