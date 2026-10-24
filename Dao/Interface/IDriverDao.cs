using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;
namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de pilotos.
    /// </summary>
    public interface IDriverDao
    {
        /// <summary>
        /// Adiciona os pilotos ao contexto para persistência.
        /// </summary>
        /// <param name="drivers">Pilotos a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> InsertDrivers(List<Driver> drivers);

        /// <summary>
        /// Marca o piloto para remoção no contexto.
        /// </summary>
        /// <param name="driver">Piloto a remover.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> DeleteDriver(Driver driver);

        /// <summary>
        /// Consulta um piloto pelo identificador do registro.
        /// </summary>
        /// <param name="id">Identificador do registro do piloto.</param>
        /// <returns>Uma tarefa que retorna uma resposta com o piloto encontrado, ou valor nulo quando não houver correspondência, ou uma indicação de falha em caso de erro.</returns>
        Task<SingleResponse<Driver>> GetDriverById(int id);

        /// <summary>
        /// Consulta todos os pilotos de uma sessão.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de pilotos da sessão, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey);

        /// <summary>
        /// Consulta o sobrenome de um piloto em uma sessão.
        /// </summary>
        /// <param name="driverNumber">Número do piloto.</param>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com o sobrenome do piloto, ou uma string vazia quando não houver correspondência, ou uma indicação de falha em caso de erro.</returns>
        Task<SingleResponse<string>> GetDriverName(int driverNumber, int sessionKey);
    }
}
