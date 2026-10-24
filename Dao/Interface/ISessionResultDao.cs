using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de resultados de sessões.
    /// </summary>
    public interface ISessionResultDao
    {
        /// <summary>
        /// Adiciona os resultados de sessões ao contexto para persistência.
        /// </summary>
        /// <param name="sessionResults">Resultados de sessões a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveSessionResults(List<SessionResult> sessionResults);

        /// <summary>
        /// Consulta os resultados de uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de resultados da sessão, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<SessionResult>> GetSessionResultsBySesssionKey(int sessionKey);
    }
}
