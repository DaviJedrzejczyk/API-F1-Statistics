using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de ultrapassagens.
    /// </summary>
    public interface IOvertakeDao
    {
        /// <summary>
        /// Adiciona as ultrapassagens ao contexto para persistência.
        /// </summary>
        /// <param name="overtakes">Ultrapassagens a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveOvertakes(List<Overtake> overtakes);
        Task<Response> SaveOvertakes(List<Overtake> overtakes, CancellationToken cancellationToken);

        /// <summary>
        /// Consulta as ultrapassagens de uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de ultrapassagens da sessão, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey);
        Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey, CancellationToken cancellationToken);
    }
}
