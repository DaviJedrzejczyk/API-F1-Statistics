# R06 v2.0.0 — catálogo de voltas para comparação

## Objetivo e escopo

Permitir a consulta de uma sessão histórica e a escolha de voltas de **exatamente três pilotos distintos** para uma comparação posterior. O catálogo deve funcionar com os dados persistidos da sessão, mesmo sem mapa ou telemetria. Este texto define comportamento de produto; a rota, os parâmetros e os formatos HTTP finais dependem do commit de desenvolvimento de R06.

## Requisitos

1. Identificar a sessão histórica e os três pilotos por suas identidades na sessão. Rejeitar pilotos repetidos. Cada piloto mantém sua própria lista e seleção; os números das três voltas escolhidas podem ser diferentes.
2. Listar **todas** as voltas persistidas de cada par sessão/piloto, identificadas pelo número da volta e ordenadas por ele. Uma volta sem duração ou sem dados auxiliares continua no catálogo. A ausência de voltas de um piloto deve aparecer explicitamente, sem criar voltas fictícias.
3. Oferecer filtros combináveis por número da volta, composto de pneu e presença de alertas. O composto é associado ao stint do mesmo piloto que abrange a volta, quando identificável. Um composto indisponível deve ser distinguível de um composto conhecido. O filtro altera a visualização, sem apagar registros ou seus alertas.
4. Expor para cada volta os dados disponíveis e sinalizar explicitamente os ausentes, incluindo duração, setores, composto e informações necessárias para classificar alertas. Não converter ausência em zero, `false`, string vazia ou em afirmação de validade. Alertas devem trazer sua natureza e a evidência disponível, por exemplo volta de entrada/saída de boxes ou exclusão de tempo identificada em dado persistido de direção de prova.
5. Para cada piloto, sugerir a volta de **menor duração positiva** entre as voltas persistidas elegíveis. Excluir da sugestão voltas identificadas como entrada ou saída de boxes e voltas com exclusão de tempo identificada. Duração ausente, zero ou negativa não é elegível. A sugestão é uma conveniência para iniciar a escolha, não uma homologação de resultado.
6. Se não houver candidata elegível, retornar a ausência de sugestão com motivo compreensível, como nenhuma volta persistida, nenhuma duração positiva ou todas as candidatas com exclusão identificada. A lista completa permanece acessível para escolha manual.
7. Permitir seleção manual de qualquer volta persistida de cada piloto, inclusive uma volta com alerta. A escolha manual não remove, oculta nem altera alertas ou dados ausentes. A comparação deve receber a identidade da volta efetivamente escolhida, sem presumir que as três escolhas compartilhem o mesmo número.

Um alerta ausente **não prova validade oficial**: pode faltar registro, cobertura ou regra para identificar uma ocorrência. A ausência de um alerta identificado e a confirmação oficial de validade são estados distintos.

## Exemplos conceituais de dados incompletos

- Uma volta persistida com número e piloto conhecidos, mas duração e setores ausentes, aparece na lista e não pode ser sugerida.
- Uma volta com duração positiva e sem stint que permita associar composto exibe composto indisponível; ela só participa de um filtro de composto conhecido se a associação for demonstrável.
- Uma volta com duração positiva e evento de exclusão de tempo identificado continua disponível para seleção manual com o alerta visível, mas fica fora da sugestão.
- Se não houver registros de direção de prova para a sessão, o catálogo não apresenta isso como confirmação de que todas as voltas são oficialmente válidas.

## Critérios de aceite

- Para uma sessão histórica com três pilotos distintos, o catálogo retorna todas as voltas persistidas de cada um por número, inclusive voltas incompletas, e permite selecionar números de volta diferentes.
- Piloto repetido não produz uma comparação de três pilotos; um piloto sem voltas tem lista vazia e motivo de ausência de sugestão.
- Filtros por número, composto e presença de alertas funcionam em combinação e não alteram o conjunto persistido; dado de composto indisponível permanece explícito.
- A sugestão de cada piloto é a menor duração positiva elegível após as exclusões identificadas de boxes e de tempo. Quando não há elegível, o motivo é apresentado; uma escolha manual preserva todos os alertas.
- Dados ausentes são representados como ausentes e a falta de alerta não é descrita como validade oficial.
- O catálogo pode ser consultado e usado sem carregar mapa ou telemetria.

## Limite desta entrega

Janelas UTC de telemetria, mapa, curva e fila de consultas OpenF1 pertencem ao contexto futuro R07–R10. Este documento não os especifica como funcionalidades implementadas em R06. A documentação final de endpoint deve ser escrita após o commit de desenvolvimento, conforme o contrato efetivamente entregue.
