using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de sessões.
    /// </summary>
    public interface ISessionDao
    {
        /// <summary>
        /// Adiciona as sessões ao contexto para persistência.
        /// </summary>
        /// <param name="sessions">Sessões a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> InsertSessions(List<Session> sessions);

        /// <summary>
        /// Consulta uma sessão pelas chaves do evento e da sessão.
        /// </summary>
        /// <param name="meetingKey">Chave do evento ao qual a sessão pertence.</param>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a sessão encontrada, ou uma indicação de falha se ela não existir, ou uma indicação de falha em caso de erro.</returns>
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);

        /// <summary>
        /// Verifica se existe alguma sessão cadastrada para o evento.
        /// </summary>
        /// <param name="meetingKey">Chave do evento consultado.</param>
        /// <returns>Uma tarefa que retorna uma resposta com uma indicação de sucesso se existir uma sessão, ou de falha se não existir ou ocorrer um erro, ou uma indicação de falha em caso de erro.</returns>
        Task<Response> GetAlreadyHaveSession(int meetingKey);
    }
}
