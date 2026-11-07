import PlaceSearch from './PlaceSearch.jsx';

export const emptyBirth = { name: '', date: '', time: '', place: null };

export const isBirthComplete = (b) => Boolean(b?.name && b.date && b.time && b.place);

/** Shape the API expects for /kundali and /matching. */
export const toBirthInput = (b) => ({
  name: b.name.trim(),
  date: b.date,
  time: b.time,
  latitude: b.place.latitude,
  longitude: b.place.longitude,
  timeZone: b.place.timeZone,
  place: [b.place.name, b.place.region, b.place.country].filter(Boolean).join(', ')
});

/** Birth details encoded in a URL so a chart can be bookmarked or shared. */
export const birthToParams = (b, prefix = '') => ({
  [`${prefix}n`]: b.name, [`${prefix}d`]: b.date, [`${prefix}t`]: b.time,
  [`${prefix}lat`]: b.place.latitude, [`${prefix}lon`]: b.place.longitude, [`${prefix}tz`]: b.place.timeZone, [`${prefix}p`]: b.place.name
});

export const birthFromParams = (params, prefix = '') => {
  const get = (k) => params.get(prefix + k);
  if (!get('n') || !get('d') || !get('t') || !get('lat') || !get('lon') || !get('tz')) return null;
  return {
    name: get('n'), date: get('d'), time: get('t'),
    place: { name: get('p') ?? '', region: '', country: '', latitude: Number(get('lat')), longitude: Number(get('lon')), timeZone: get('tz') }
  };
};

/** Name, date, time and place fields. Controlled: pass value and onChange. */
export default function BirthForm({ value, onChange, idPrefix = 'b', legend }) {
  const set = (key) => (e) => onChange({ ...value, [key]: e.target.value });
  return (
    <fieldset className="birth-form">
      {legend && <legend>{legend}</legend>}
      <div className="field">
        <label htmlFor={`${idPrefix}-name`}>Name</label>
        <input id={`${idPrefix}-name`} value={value.name} onChange={set('name')} required maxLength={80} autoComplete="name" />
      </div>
      <div className="field-row">
        <div className="field">
          <label htmlFor={`${idPrefix}-date`}>Date of birth</label>
          <input id={`${idPrefix}-date`} type="date" value={value.date} onChange={set('date')} required min="1800-01-01" max="2100-12-31" />
        </div>
        <div className="field">
          <label htmlFor={`${idPrefix}-time`}>Time of birth</label>
          <input id={`${idPrefix}-time`} type="time" value={value.time} onChange={set('time')} required />
        </div>
      </div>
      <PlaceSearch id={`${idPrefix}-place`} value={value.place} onChange={(place) => onChange({ ...value, place })} />
    </fieldset>
  );
}
