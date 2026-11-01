using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de setores rápidos de voltas.
    /// </summary>
    public interface ILapFastSectorDao
    {
        /// <summary>
        /// Adiciona os setores rápidos de voltas ao contexto para persistência.
        /// </summary>
        /// <param name="lapFastSectors">Setores rápidos a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveLapFastSector(List<LapFastSector> lapFastSectors);
        Task<Response> UpsertLapFastSectors(List<LapFastSector> lapFastSectors, CancellationToken cancellationToken);

        /// <summary>
        /// Consulta todos os setores rápidos de voltas de uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de setores rápidos da sessão, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<LapFastSector>> GetAllLapFastSectorsSession(int sessionKey);
        Task<DataResponse<LapFastSector>> GetAllLapFastSectorsSession(int sessionKey, CancellationToken cancellationToken);
    }
}
