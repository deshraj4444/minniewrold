const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api';

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers ?? {})
    },
    ...options
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const api = {
  getDailyHoroscope: (sign) => request(`/horoscopes/daily/${sign}`),
  getBlogs: (includeDrafts = false) => request(`/blogs?includeDrafts=${includeDrafts}`),
  createBlog: (payload) => request('/admin/blogs', { method: 'POST', body: JSON.stringify(payload) }),
  updateBlog: (id, payload) => request(`/admin/blogs/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deleteBlog: (id) => request(`/admin/blogs/${id}`, { method: 'DELETE' }),
  getProducts: () => request('/products'),
  getTestimonials: () => request('/testimonials'),
  submitContact: (payload) => request('/contact', { method: 'POST', body: JSON.stringify(payload) }),
  submitKundali: (payload) => request('/kundali-requests', { method: 'POST', body: JSON.stringify(payload) })
};
