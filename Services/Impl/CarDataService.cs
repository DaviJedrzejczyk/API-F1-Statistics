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

        public async Task<DataResponse<CarData>> GetHighSpeedsSessionApi(int sessionKey, int minimunSpeed)
            => await GetHighSpeedsSessionApi(sessionKey, minimunSpeed, CancellationToken.None);

        public async Task<DataResponse<CarData>> GetHighSpeedsSessionApi(int sessionKey, int minimunSpeed, CancellationToken cancellationToken)
        {
            try
            {
                var session = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var key = session + ":" + minimunSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<CarData>(_unityOfWork, "car_data", key, cancellationToken);
                if (cached != null) return WindowResult(cached);
                DataResponse<CarData> response = await _carDataClient.GetHighSpeedsSession(sessionKey, minimunSpeed, cancellationToken);
                if (!response.HasSuccess || response.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(response.Message, response.Exception);
                if (response.Itens.Any(x => x.SessionKey != sessionKey || x.Speed < minimunSpeed))
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>("Invalid car data window returned by upstream.");
                List<CarData> carDatas = response.Itens.GroupBy(x => x.DriverNumber).Select(x => x.MaxBy(y => y.Speed)!).ToList();
                await using var transaction = await _unityOfWork.BeginImport("car_data", session, cancellationToken);
                cached = await ImportWindowCache.Read<CarData>(_unityOfWork, "car_data", key, cancellationToken);
                if (cached != null) return WindowResult(cached);
                var responseDatabase = await _unityOfWork.CarDataDao.GetHighSpeedSessionDatabase(sessionKey, minimunSpeed, cancellationToken);
                if (!responseDatabase.HasSuccess || responseDatabase.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(responseDatabase.Message, responseDatabase.Exception);
                var existing = responseDatabase.Itens.Select(x => (x.SessionKey, x.DriverNumber, x.Date)).ToHashSet();
                var missing = carDatas.Where(x => !existing.Contains((x.SessionKey, x.DriverNumber, x.Date))).ToList();
                if (missing.Count > 0)
                {
                    var insert = await _unityOfWork.CarDataDao.SaveCarDatas(missing, cancellationToken);
                    if (!insert.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(insert.Message, insert.Exception);
                }
                await ImportWindowCache.Stage(_unityOfWork, "car_data", key, carDatas, cancellationToken);
                var commit = await _unityOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return WindowResult(carDatas);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(ex);
            }
        }

        private static DataResponse<CarData> WindowResult(List<CarData> items)
            => items.Count == 0
                ? ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>("Not found the maximum speed of the drivers")
                : ResponseFactory.CreateInstance().CreateSuccessDataResponse(items);

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
            => await GetSortedHighSpeedsSession(sessionKey, minimunSpeed, CancellationToken.None);

        public async Task<DataResponse<SessionDriverSpeedDTO>> GetSortedHighSpeedsSession(int sessionKey, int minimunSpeed, CancellationToken cancellationToken)
        {
            DataResponse<Driver> driversList = await _driverService.GetAllDriversSession(sessionKey, cancellationToken);

            if (!driversList.HasSuccess || driversList.Itens == null || driversList.Itens.Count <= 0)
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionDriverSpeedDTO>("Drivers must be inserted.");

            DataResponse<CarData> carDataResponse = await GetHighSpeedsSessionApi(sessionKey, minimunSpeed, cancellationToken);
            
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

        public async Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed)
        {
            try
            {
                DataResponse<CarData> dataResponse = await _unityOfWork.CarDataDao.GetHighSpeedSessionDatabase(sessionKey, minimunSpeed);
                
                if (!dataResponse.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(dataResponse.Message, dataResponse.Exception);

                return dataResponse;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(ex);
            }
        }
    }
}
