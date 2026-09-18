using Dao.Interface;
using Entities.Class;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class LapFastSectorService : ILapFastSectorService
    {
        private readonly IUnityOfWork _unityOfWork;

        public LapFastSectorService(IUnityOfWork unityOfWork)
        {
            _unityOfWork = unityOfWork;
        }

        public async Task<DataResponse<LapFastSector>> GetFastSectorsOfSession(int sessionKey)
        {
            try
            {
                var lapFastSectors = await _unityOfWork.LapFastSectorDao.GetAllLapFastSectorsSession(sessionKey);

                if (lapFastSectors.HasSuccess)
                {
                    var response = await SaveFastSectors(lapFastSectors.Itens);

                    if (!response.HasSuccess)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(response.Message, response.Exception);

                    return lapFastSectors;
                }

                if (lapFastSectors.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(lapFastSectors.Message, lapFastSectors.Exception);

                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>("The fastest sectors could not be retrieved or not found.");

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(ex);
            }
        }

        public async Task<Response> SaveFastSectors(List<LapFastSector> lapFastSectors)
        {
            try
            {
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
