using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de telemetria dos carros.
    /// </summary>
    public interface ICarDataDao
    {
        /// <summary>
        /// Adiciona os dados de telemetria dos carros ao contexto para persistência.
        /// </summary>
        /// <param name="data">Dados de telemetria a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveCarDatas(List<CarData> data);

        /// <summary>
        /// Consulta a telemetria da sessão com velocidade maior ou igual ao limite informado.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <param name="minimunSpeed">Velocidade mínima, em km/h, incluída na consulta.</param>
        /// <returns>Uma tarefa que retorna uma resposta com os dados de telemetria que atendem ao filtro, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed);
    }
}
