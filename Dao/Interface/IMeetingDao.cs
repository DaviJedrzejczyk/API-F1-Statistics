using Entities.Class;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de eventos do campeonato.
    /// </summary>
    public interface IMeetingDao
    {
        /// <summary>
        /// Adiciona os eventos recebidos ao contexto para persistência.
        /// </summary>
        /// <param name="meetings">Eventos a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> InsertTracksOfCurrentYear(List<Meeting> meetings);

        /// <summary>
        /// Consulta os eventos cujo nome contém Grand Prix no ano informado, ordenados pela data de início.
        /// </summary>
        /// <param name="year">Ano dos eventos consultados.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de eventos ordenada pela data de início, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year);

        /// <summary>
        /// Consulta um evento pela sua chave.
        /// </summary>
        /// <param name="meetingKey">Chave do evento consultado.</param>
        /// <returns>Uma tarefa que retorna uma resposta com o evento encontrado, ou valor nulo quando não houver correspondência, ou uma indicação de falha em caso de erro.</returns>
        Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey);

        /// <summary>
        /// Consulta a chave do primeiro evento não cancelado, por data de início, cujo término ocorre entre agora e os próximos sete dias em UTC.
        /// </summary>
        /// <returns>Uma tarefa que retorna uma resposta com a chave do evento encontrado, ou zero quando não houver correspondência, ou uma indicação de falha em caso de erro.</returns>
        Task<SingleResponse<int>> GetRecentMeetingKey();
    }
}
