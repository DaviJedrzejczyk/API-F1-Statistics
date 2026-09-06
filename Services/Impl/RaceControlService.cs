using Dao.Interface;
using Entities;
using Entities.Dtos.RaceControlDTOs;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class RaceControlService : IRaceControlService
    {
        private readonly IRaceControlClient _race;
        private readonly IUnityOfWork _unitOfWork;

        public RaceControlService(IRaceControlClient race, IUnityOfWork unitOfWork)
        {
            _race = race;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResponse<RaceControlFilterDto>> GetRaceControlsBySessionFlags(int sessionKey, string[] flags)
        {
            try
            {
                var responseApi = await GetRaceControlsBySessionFlagsApi(sessionKey, flags);
                if (!responseApi.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControlFilterDto>(responseApi.Message, responseApi.Exception);

                return CreateRaceControlListDto(responseApi.Itens);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControlFilterDto>(ex);
            }
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsApi(int sessionKey, string[] flags)
        {
            try
            {
                var responseDb = await GetRaceControlsBySessionFlagsDb(sessionKey, flags);
                if (!responseDb.HasSuccess) return responseDb;

                if (responseDb.Itens != null && responseDb.Itens.Count > 0)
                    return responseDb;

                DataResponse<RaceControl> response = await _race.GetRaceControlsBySessionFlags(sessionKey, flags);
                if (!response.HasSuccess || response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>("No race controls found for the specified session.");

                return response;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(ex);
            }
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsDb(int sessionKey, string[] flags)
        {
            try
            {
                return await _unitOfWork.RaceControlDao.GetRaceControlsBySessionFlags(sessionKey, flags);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(ex);
            }
        }

        public async Task<Response> SaveRaceControls(List<RaceControl> raceControls)
        {
            try
            {
                var response = await _unitOfWork.RaceControlDao.SaveRaceControls(raceControls);
                if (!response.HasSuccess)
                    return response;

                return await _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        private DataResponse<RaceControlFilterDto> CreateRaceControlListDto(List<RaceControl> raceControls)
        {
            try
            {
                List<RaceControlFilterDto> raceControlDtos = [];
                for (int i = 0; i < raceControls.Count; i++)
                {
                    var raceControl = raceControls[i];

                    if (raceControl.Flag != "YELLOW" && raceControl.Flag != "DOUBLE YELLOW")
                        continue;

                    var nextRaceControl = raceControls.Skip(i + 1)
                                            .FirstOrDefault(rc => rc.Sector == raceControl.Sector && (rc.Flag != "YELLOW" && rc.Flag != "DOUBLE YELLOW"));
                    
                    if (nextRaceControl != null)
                    {
                        raceControlDtos.Add(new RaceControlFilterDto
                        {
                            Sector = raceControl.Sector ?? nextRaceControl.Sector,
                            Flag = raceControl.Flag,
                            DateStart = raceControl.Date,
                            DateEnd = nextRaceControl.Date
                        });
                    }
                }

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(raceControlDtos);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControlFilterDto>(ex);
            }
        }
    }
}
