using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de registros da direção de prova.
    /// </summary>
    public interface IRaceControlDao
    {
        /// <summary>
        /// Consulta os registros da direção de prova de uma sessão cujas bandeiras correspondem às informadas.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <param name="sessionFlags">Bandeiras aceitas pelo filtro.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de registros que atendem aos filtros, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlags(int sessionKey, string[] sessionFlags);

        /// <summary>
        /// Adiciona os registros da direção de prova ao contexto para persistência.
        /// </summary>
        /// <param name="raceControls">Registros da direção de prova a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveRaceControls(List<RaceControl> raceControls);
    }
}
