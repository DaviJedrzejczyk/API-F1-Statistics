using Dao.Interface;
using Entities;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class SessionService : ISessionService
    {
        private readonly ISessionDao _sessionDao;
        public SessionService(ISessionDao sessionDao)
        {
            this._sessionDao = sessionDao;
        }

        public async Task<Response> InsertTracksOfCurrentYear(List<Session> sessions)
        {
            try
            {
                //Todo: Validar se todas as pistas passadas na lista já estão na base.
                //      Interessante seria buscar todas as pistas do ano corrente e comparar com a lista passada, se houver alguma pista que não esteja na base, inserir.
                //      A lista sessions tem o método Remove, que pede uma entidade Session, então iterando por essa lista e comparando entidade a entidade com o banco,
                //      remover quais tem a mesma chave. Se no ano dessa lista não tiver nada na base nem passa pelo for.
                return await _sessionDao.InsertTracksOfCurrentYear(sessions);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
