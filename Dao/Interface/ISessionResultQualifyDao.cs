using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define as operações de acesso a dados de resultados de classificação.
    /// </summary>
    public interface ISessionResultQualifyDao
    {
        /// <summary>
        /// Adiciona os resultados de classificação ao contexto para persistência.
        /// </summary>
        /// <param name="sessionResultQualifiess">Resultados de classificação a adicionar.</param>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        /// <remarks>A alteração é persistida ao executar <see cref="IUnityOfWork.Commit"/>.</remarks>
        Task<Response> SaveQualifyResult(List<SessionResultQualify> sessionResultQualifiess);

        /// <summary>
        /// Consulta os resultados de classificação de uma sessão, ordenados por fase e diferença para o líder.
        /// </summary>
        /// <param name="sessionKey">Chave da sessão consultada.</param>
        /// <returns>Uma tarefa que retorna uma resposta com a lista de resultados ordenada por fase e diferença para o líder, ou uma indicação de falha em caso de erro.</returns>
        Task<DataResponse<SessionResultQualify>> GetQualifyingResultBySessionKey(int sessionKey);
    }
}
