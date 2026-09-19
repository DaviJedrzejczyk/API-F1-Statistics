using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using Services.Interfaces;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class LapFastSectorService : ILapFastSectorService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IDriverService _driverService;

        public LapFastSectorService(IUnityOfWork unityOfWork, IDriverService driverService)
        {
            _unityOfWork = unityOfWork;
            _driverService = driverService;
        }

        public async Task<DataResponse<LapFastSector>> CreateListWithFastSector(List<LapListFastSector> listFastSectorAllDrivers, int sessionKey, List<LapFastSector> listFastSector)
        {
            try
            {
                var bestSector1 = listFastSectorAllDrivers
                    .Where(x => x.Sector1Duration > 0)
                    .MinBy(x => x.Sector1Duration);

                var bestSector2 = listFastSectorAllDrivers
                    .Where(x => x.Sector2Duration > 0)
                    .MinBy(x => x.Sector2Duration);

                var bestSector3 = listFastSectorAllDrivers
                    .Where(x => x.Sector3Duration > 0)
                    .MinBy(x => x.Sector3Duration);

                if (bestSector1 != null)
                {
                    var driverName = await _driverService.GetDriverName(bestSector1.DriverNumber, sessionKey);

                    if (!driverName.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(driverName.Message, driverName.Exception);

                    listFastSector.Add(new LapFastSector
                    {
                        DriverNumber = bestSector1.DriverNumber,
                        Duration = bestSector1.Sector1Duration,
                        DriverName = driverName.Item,
                        Sector = 1,
                        SessionKey = sessionKey
                    });
                }

                if (bestSector2 != null)
                {
                    var driverName = await _driverService.GetDriverName(bestSector2.DriverNumber, sessionKey);

                    if (!driverName.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(driverName.Message, driverName.Exception);

                    listFastSector.Add(new LapFastSector
                    {
                        DriverNumber = bestSector2.DriverNumber,
                        Duration = bestSector2.Sector2Duration,
                        DriverName = driverName.Item,
                        Sector = 2,
                        SessionKey = sessionKey
                    });
                }

                if (bestSector3 != null)
                {
                    var driverName = await _driverService.GetDriverName(bestSector3.DriverNumber, sessionKey);

                    if (!driverName.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(driverName.Message, driverName.Exception);

                    listFastSector.Add(new LapFastSector
                    {
                        DriverNumber = bestSector3.DriverNumber,
                        Duration = bestSector3.Sector3Duration,
                        DriverName = driverName.Item,
                        Sector = 3,
                        SessionKey = sessionKey
                    });
                }

                if (listFastSector.Count == 0) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>("List of fatests sectos cannot be created!");

                var response = await SaveFastSectors(listFastSector, listFastSector.Count < 3 ? "Not all fast sectors were found." : string.Empty);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(response.Message, response.Exception);
                

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(listFastSector, listFastSector.Count < 3 ? "Not all fast sectors were found." : string.Empty);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(ex);
            }
        }

        public async Task<DataResponse<LapFastSector>> GetFastSectorsOfSession(int sessionKey)
        {
            try
            {
                var lapFastSectors = await _unityOfWork.LapFastSectorDao.GetAllLapFastSectorsSession(sessionKey);

                if (lapFastSectors.HasSuccess) return lapFastSectors;

                if (lapFastSectors.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(lapFastSectors.Message, lapFastSectors.Exception);

                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>("The fastest sectors could not be retrieved or not found.");

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(ex);
            }
        }

        public async Task<Response> SaveFastSectors(List<LapFastSector> lapFastSectors, string observation)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(observation))
                {
                    foreach (var item in lapFastSectors)
                        item.Observation = observation;
                }
                
                var response = await _unityOfWork.LapFastSectorDao.SaveLapFastSector(lapFastSectors);
                
                if (!response.HasSuccess) return response;

                response = await _unityOfWork.Commit();

                if (!response.HasSuccess) return response;

                return ResponseFactory.CreateInstance().CreateSuccessResponse("The fastest sectors were saved successfully.");
            }
            catch (Exception ex)
            {   
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
