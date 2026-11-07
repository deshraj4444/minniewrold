import DragSort from './DragSort.jsx';
import LabelDiagram from './LabelDiagram.jsx';
import MatchPairs from './MatchPairs.jsx';
import NumberLine from './NumberLine.jsx';
import SpeedQuiz from './SpeedQuiz.jsx';
import Timeline from './Timeline.jsx';
import './games.css';

const components = { dragSort: DragSort, matchPairs: MatchPairs, timeline: Timeline, labelDiagram: LabelDiagram, numberLine: NumberLine, speedQuiz: SpeedQuiz };

/** One game from a chapter file, picked by its "type". `storageKey` keeps best scores apart per chapter. */
export default function Game({ game, lang, storageKey }) {
  const Component = components[game.type];
  return Component ? <Component game={game} lang={lang} storageKey={storageKey} /> : null;
}
