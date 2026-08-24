using Dao.Interface;
using Entities;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class CarDataService : ICarDataService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly ICarDataClient _carDataClient;
        private readonly IDriverService _driverService;

        public CarDataService(IUnityOfWork unityOfWork, ICarDataClient carDataClient, IDriverService driverService)
        {
            _unityOfWork = unityOfWork;
            _carDataClient = carDataClient;
            _driverService = driverService;
        }

        public async Task<SingleResponse<CarData>> GetHighSpeedDriverSession(int sessionKey, int driverNumber, int minimunSpeed)
        {
            try
            {
                DataResponse<CarData> response = await _carDataClient.GetHighSpeedsDriverSession(sessionKey, driverNumber, minimunSpeed);

                if (!response.HasSuccess || response.Itens.Count <= 0)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<CarData>(response.Message, response.Exception);

                CarData? carData = response.Itens.MaxBy(x => x.Speed);

                Response insertResponse = await SaveCarData(carData);

                if (!insertResponse.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureSingleResponse<CarData>(insertResponse.Message, insertResponse.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<CarData>(carData);

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<CarData>(ex);
            }
        }

        public async Task<Response> SaveCarData(CarData data)
        {
            try
            {
                Response response = await _unityOfWork.CarDataDao.SaveCarData(data);
                
                if (!response.HasSuccess)
                    return response;

                return await _unityOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);   
            }
        }

        public async Task<DataResponse<SessionDriverSpeedDTO>> GetSortedHighSpeedsSession(int sessionKey, int meetingKey, int minimunSpeed)
        {
            DataResponse<Driver> driversList = await _driverService.GetAllDriversSession(meetingKey, sessionKey);

            if (driversList.Itens.Count <= 0)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionDriverSpeedDTO>("Drivers must be inserted");

            List<SessionDriverSpeedDTO> speedDTOs = [];
            for (int i = 0; i <= driversList.Itens.Count; i++)
            {
                SingleResponse<CarData> carDataResponse = await GetHighSpeedDriverSession(meetingKey, sessionKey, minimunSpeed);
                speedDTOs.Add(new SessionDriverSpeedDTO()
                {
                    DriverName = driversList.Itens[i].LastName,
                    MeetingKey = meetingKey,
                    SessionKey = sessionKey,
                    DriverNumber = driversList.Itens[i].DriverNumber,
                    Speed = carDataResponse.Item.Speed,
                });
            }

            return ResponseFactory.CreateInstance().CreateSuccessDataResponse(speedDTOs);
        }
    }
}
