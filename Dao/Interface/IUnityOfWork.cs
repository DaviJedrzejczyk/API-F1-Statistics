using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    /// <summary>
    /// Define o acesso aos DAOs e a persistência das alterações da unidade de trabalho.
    /// </summary>
    public interface IUnityOfWork
    {
        /// <summary>
        /// Obtém o DAO de sessões.
        /// </summary>
        /// <value>A instância de <see cref="ISessionDao"/> da unidade de trabalho.</value>
        ISessionDao SessionDao { get; }
        /// <summary>
        /// Obtém o DAO de eventos do campeonato.
        /// </summary>
        /// <value>A instância de <see cref="IMeetingDao"/> da unidade de trabalho.</value>
        IMeetingDao MeetingDao { get; }
        /// <summary>
        /// Obtém o DAO de pilotos.
        /// </summary>
        /// <value>A instância de <see cref="IDriverDao"/> da unidade de trabalho.</value>
        IDriverDao DriverDao { get; }
        /// <summary>
        /// Obtém o DAO de dados de telemetria dos carros.
        /// </summary>
        /// <value>A instância de <see cref="ICarDataDao"/> da unidade de trabalho.</value>
        ICarDataDao CarDataDao { get; }
        /// <summary>
        /// Obtém o DAO de resultados de sessões.
        /// </summary>
        /// <value>A instância de <see cref="ISessionResultDao"/> da unidade de trabalho.</value>
        ISessionResultDao SessionResultDao { get; }
        /// <summary>
        /// Obtém o DAO de ultrapassagens.
        /// </summary>
        /// <value>A instância de <see cref="IOvertakeDao"/> da unidade de trabalho.</value>
        IOvertakeDao OvertakeDao { get; }
        /// <summary>
        /// Obtém o DAO de paradas nos boxes.
        /// </summary>
        /// <value>A instância de <see cref="IPitDao"/> da unidade de trabalho.</value>
        IPitDao PitDao { get; }
        /// <summary>
        /// Obtém o DAO de registros da direção de prova.
        /// </summary>
        /// <value>A instância de <see cref="IRaceControlDao"/> da unidade de trabalho.</value>
        IRaceControlDao RaceControlDao { get; }
        /// <summary>
        /// Obtém o DAO de stints (sequências de voltas com um jogo de pneus).
        /// </summary>
        /// <value>A instância de <see cref="IStintDao"/> da unidade de trabalho.</value>
        IStintDao StintDao { get; }
        /// <summary>
        /// Obtém o DAO de resultados de classificação.
        /// </summary>
        /// <value>A instância de <see cref="ISessionResultQualifyDao"/> da unidade de trabalho.</value>
        ISessionResultQualifyDao SessionResultQualifyDao { get; }
        /// <summary>
        /// Obtém o DAO de voltas.
        /// </summary>
        /// <value>A instância de <see cref="ILapDao"/> da unidade de trabalho.</value>
        ILapDao LapDao { get; }
        /// <summary>
        /// Obtém o DAO de segmentos de voltas.
        /// </summary>
        /// <value>A instância de <see cref="ILapSegmentDao"/> da unidade de trabalho.</value>
        ILapSegmentDao LapSegmentDao { get; }
        /// <summary>
        /// Obtém o DAO de setores rápidos de voltas.
        /// </summary>
        /// <value>A instância de <see cref="ILapFastSectorDao"/> da unidade de trabalho.</value>
        ILapFastSectorDao LapFastSectorDao { get; }

        /// <summary>
        /// Persiste no banco de dados as alterações pendentes no contexto da unidade de trabalho.
        /// </summary>
        /// <returns>Uma tarefa que retorna uma resposta indicando o sucesso ou a falha da operação.</returns>
        Task<Response> Commit();
    }
}
