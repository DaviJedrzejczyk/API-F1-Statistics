using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de stints (sequências de voltas com um jogo de pneus).
    /// </summary>
    public interface IStintDao
    {
        /// <summary>
        /// Consulta os stints de uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de stints da sessão, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey);
        Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey, CancellationToken cancellationToken);

        /// <summary>
        /// Adiciona os stints ao contexto para persistência.
        /// </summary>
        /// <param name="stints">Stints a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveStints(List<Stint> stints);
        Task<Response> SaveStints(List<Stint> stints, CancellationToken cancellationToken);
    }
}
