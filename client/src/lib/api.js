export class ApiError extends Error {
  constructor(message, status, errors) {
    super(message);
    this.status = status;
    this.errors = errors ?? {};
  }
}

async function request(path, { method = 'GET', body, form } = {}) {
  const headers = { 'X-Requested-With': 'fetch' };
  let payload;
  if (form) {
    payload = form;
  } else if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
    payload = JSON.stringify(body);
  }

  const response = await fetch(`/api${path}`, { method, headers, body: payload, credentials: 'same-origin' });
  if (response.status === 204) return null;

  const isJson = response.headers.get('content-type')?.includes('json');
  const data = isJson ? await response.json() : await response.text();
  if (!response.ok) {
    const errors = data?.errors ?? {};
    const firstError = Object.values(errors).flat()[0];
    const message = data?.message ?? firstError ?? data?.detail ?? data?.title ?? `Request failed (${response.status})`;
    throw new ApiError(message, response.status, errors);
  }

  return data;
}

const qs = (params) => {
  const s = new URLSearchParams(Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== '')).toString();
  return s ? `?${s}` : '';
};

export const api = {
  get: (path) => request(path),
  post: (path, body) => request(path, { method: 'POST', body }),
  put: (path, body) => request(path, { method: 'PUT', body }),
  del: (path) => request(path, { method: 'DELETE' }),
  upload: (file) => {
    const form = new FormData();
    form.append('file', file);
    return request('/admin/uploads', { method: 'POST', form });
  },

  sites: () => request('/sites'),
  contact: (body) => request('/contact', { method: 'POST', body }),

  astro: {
    horoscope: (sign, period = 'daily') => request(`/astro/horoscopes/${sign}${qs({ period })}`),
    places: (q) => request(`/astro/places${qs({ q })}`),
    panchang: (params = {}) => request(`/astro/panchang${qs(params)}`),
    kundali: (body) => request('/astro/kundali', { method: 'POST', body }),
    matching: (body) => request('/astro/matching', { method: 'POST', body }),
    numerology: (name, dob) => request(`/astro/numerology${qs({ name, dob })}`),
    testimonials: () => request('/astro/testimonials'),
    consultation: (body) => request('/astro/consultations', { method: 'POST', body }),
    varshphal: (birth, year) => request('/astro/varshphal', { method: 'POST', body: { birth, year } }),
    contactInfo: () => request('/astro/contact-info')
  },

  students: {
    me: () => request('/students/me'),
    register: (body) => request('/students/register', { method: 'POST', body }),
    login: (body) => request('/students/login', { method: 'POST', body }),
    logout: () => request('/students/logout', { method: 'POST' }),
    attempts: () => request('/students/me/attempts')
  },

  exams: {
    list: (params = {}) => request(`/exams${qs(params)}`),
    get: (slug) => request(`/exams/${slug}`),
    certificate: (code) => request(`/exams/certificates/${encodeURIComponent(code)}`),
    practice: (category, count = 10) => request(`/exams/practice${qs({ category, count })}`),
    /** Every question with its solution, for the free JEE chapter tests (shown on their public page). */
    jeeSolutions: (slug) => request(`/jee/solutions/${slug}`),
    start: (slug) => request(`/exams/${slug}/attempts`, { method: 'POST' }),
    purchase: (slug, upiRef) => request(`/exams/${slug}/purchase`, { method: 'POST', body: { upiRef } }),
    attempt: (id) => request(`/attempts/${id}`),
    save: (id, answers) => request(`/attempts/${id}/answers`, { method: 'PUT', body: { answers } }),
    submit: (id, answers) => request(`/attempts/${id}/submit`, { method: 'POST', body: { answers } })
  },

  /** CBSE chapters (notes, games, practice), read from the chapter files on the server. */
  cbse: {
    classes: () => request('/cbse'),
    subject: (n, subject) => request(`/cbse/class-${n}/${subject}`),
    chapter: (n, subject, slug) => request(`/cbse/class-${n}/${subject}/${slug}`)
  },
  siteInfo: () => request('/site-info'),
  blog: (blog) => ({
    categories: () => request(`/blogs/${blog}/categories`),
    posts: (params = {}) => request(`/blogs/${blog}/posts${qs(params)}`),
    post: (slug) => request(`/blogs/${blog}/posts/${slug}`),
    courses: (params = {}) => request(`/blogs/${blog}/courses${qs(params)}`),
    course: (slug) => request(`/blogs/${blog}/courses/${slug}`),
    lesson: (slug, lessonSlug) => request(`/blogs/${blog}/courses/${slug}/lessons/${lessonSlug}`)
  })
};
