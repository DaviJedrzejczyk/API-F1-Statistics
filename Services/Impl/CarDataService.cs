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

        public async Task<DataResponse<CarData>> GetHighSpeedsSession(int sessionKey, int minimunSpeed)
        {
            try
            {
                DataResponse<CarData> response = await _carDataClient.GetHighSpeedsSession(sessionKey, minimunSpeed);

                if (!response.HasSuccess || response.Itens.Count <= 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(response.Message, response.Exception);

                List<CarData> carDatas = response.Itens.GroupBy(x => x.DriverNumber).Select(x => x.MaxBy(y => y.Speed)!).ToList();

                Response insertResponse = await SaveCarDatas(carDatas);

                if (!insertResponse.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(insertResponse.Message, insertResponse.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(carDatas);

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(ex);
            }
        }

        public async Task<Response> SaveCarDatas(List<CarData> data)
        {
            try
            {
                Response response = await _unityOfWork.CarDataDao.SaveCarDatas(data);

                if (!response.HasSuccess)
                    return response;

                return await _unityOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<DataResponse<SessionDriverSpeedDTO>> GetSortedHighSpeedsSession(int sessionKey, int minimunSpeed)
        {
            DataResponse<Driver> driversList = await _driverService.GetAllDriversSession(sessionKey);

            if (driversList.Itens.Count <= 0)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionDriverSpeedDTO>("Drivers must be inserted.");

            DataResponse<CarData> carDataResponse = await GetHighSpeedsSession(sessionKey, minimunSpeed);
            
            if (carDataResponse.Itens == null) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionDriverSpeedDTO>("Not found the maximum speed of the drivers");
            
            List<SessionDriverSpeedDTO> speedDTOs = JoinListToCreateDTO(sessionKey, driversList, carDataResponse);
            speedDTOs.Sort((x,y) => y.Speed.CompareTo(x.Speed));

            return ResponseFactory.CreateInstance().CreateSuccessDataResponse(speedDTOs);
        }

        private List<SessionDriverSpeedDTO> JoinListToCreateDTO(int sessionKey, DataResponse<Driver> driversList, DataResponse<CarData> carDataResponse)
        {
            return driversList.Itens.Join(carDataResponse.Itens,
                                         driver => new { driver.DriverNumber, driver.SessionKey },
                                         carData => new { carData.DriverNumber, carData.SessionKey },
                                         (driver, carData) => new SessionDriverSpeedDTO
                                         {
                                             DriverNumber = driver.DriverNumber,
                                             DriverName = driver.LastName,
                                             Speed = carData.Speed,
                                             SessionKey = sessionKey,
                                             HeadshotUrl = driver.HeadshotUrl,
                                             TeamColour = driver.TeamColour,
                                             TeamName = driver.TeamName,
                                         })
                                   .ToList();
        }
    }
}
