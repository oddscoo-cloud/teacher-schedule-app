const API_BASE_URL =
  process.env.REACT_APP_API_URL || "https://localhost:7151/api";

const request = async (path, options = {}) => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
      ...options.headers,
    },
  });

  if (!response.ok) {
    const body = await response.text();
    let message = `Request failed (${response.status})`;
    if (body) {
      try {
        message = JSON.parse(body).message || body;
      } catch {
        message = body;
      }
    }
    throw new Error(message);
  }

  if (response.status === 204) return null;
  return response.json();
};

const jsonRequest = (method, body) => ({
  method,
  body: JSON.stringify(body),
});

export const fetchStudents = ({ search, course, status } = {}) => {
  const params = new URLSearchParams();
  if (search) params.set("search", search);
  if (course) params.set("course", course);
  if (status) params.set("status", status);
  const query = params.toString();
  return request(`/students${query ? `?${query}` : ""}`);
};

export const fetchStudent = (id) => request(`/students/${id}`);
export const createStudent = (student) =>
  request("/students", jsonRequest("POST", student));
export const updateStudent = (id, student) =>
  request(`/students/${id}`, jsonRequest("PUT", student));

export const fetchCourses = () => request("/courses");
export const createCourse = (course) =>
  request("/courses", jsonRequest("POST", course));
export const updateCourse = (id, course) =>
  request(`/courses/${id}`, jsonRequest("PUT", course));
export const deleteCourse = (id) =>
  request(`/courses/${id}`, { method: "DELETE" });

export const fetchNotes = (studentId) =>
  request(`/students/${studentId}/notes`);
export const createNote = (studentId, note) =>
  request(`/students/${studentId}/notes`, jsonRequest("POST", note));
export const updateNote = (studentId, noteId, note) =>
  request(`/students/${studentId}/notes/${noteId}`, jsonRequest("PUT", note));
export const deleteNote = (studentId, noteId) =>
  request(`/students/${studentId}/notes/${noteId}`, { method: "DELETE" });

export default API_BASE_URL;
