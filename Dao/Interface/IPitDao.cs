using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de paradas nos boxes.
    /// </summary>
    public interface IPitDao
    {
        /// <summary>
        /// Adiciona as paradas nos boxes ao contexto para persistência.
        /// </summary>
        /// <param name="pits">Paradas nos boxes a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SavePits(List<Pit> pits);
        Task<Response> SavePits(List<Pit> pits, CancellationToken cancellationToken);

        /// <summary>
        /// Consulta todas as paradas nos boxes de uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de paradas nos boxes da sessão, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<Pit>> GetAllPitsBySessionKey(int sessionKey);
        Task<DataResponse<Pit>> GetAllPitsBySessionKey(int sessionKey, CancellationToken cancellationToken);
    }
}
