using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de voltas.
    /// </summary>
    public interface ILapDao
    {
        /// <summary>
        /// Consulta a primeira volta marcada como volta rápida em uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a volta rápida encontrada, ou valor nulo quando não houver correspondência, ou uma indicação de falha em caso de erro.</returns>
        Task<SingleResponse<Lap>> GetFastLapSessionBySessionKey(int sessionKey);

        /// <summary>
        /// Adiciona uma volta ao contexto para persistência.
        /// </summary>
        /// <param name="lap">Volta a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveLap(Lap lap);
        Task<Response> UpsertLap(Lap lap, CancellationToken cancellationToken);
    }
}
