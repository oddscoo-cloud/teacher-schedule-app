const API_BASE_URL =
  process.env.REACT_APP_API_URL || "https://localhost:7151/api";

const request = async (path, options = {}) => {
  const isFormData = options.body instanceof FormData;
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      ...(isFormData ? {} : { "Content-Type": "application/json" }),
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

const requestBlob = async (path) => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
    },
  });
  if (!response.ok) {
    throw new Error(`Request failed (${response.status})`);
  }
  return response.blob();
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
export const fetchStudentFinance = (id) => request(`/students/${id}/finance`);
export const setStudentPrice = (id, price) =>
  request(`/students/${id}/finance/prices`, jsonRequest("POST", price));
export const recordStudentPayment = (id, payment) =>
  request(`/students/${id}/finance/payments`, jsonRequest("POST", payment));
export const adjustStudentPayment = (studentId, paymentId, adjustment) =>
  request(
    `/students/${studentId}/finance/payments/${paymentId}/adjustments`,
    jsonRequest("POST", adjustment),
  );
export const createStudent = (student) =>
  request("/students", jsonRequest("POST", student));
export const updateStudent = (id, student) =>
  request(`/students/${id}`, jsonRequest("PUT", student));
export const deleteStudent = (id) =>
  request(`/students/${id}`, { method: "DELETE" });
export const switchWorkspace = (membershipId) =>
  request(`/auth/switch-workspace/${membershipId}`, { method: "POST" });

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

export const fetchHomeworks = ({ studentId, courseId, pendingOnly } = {}) => {
  const params = new URLSearchParams();
  if (studentId) params.set("studentId", studentId);
  if (courseId) params.set("courseId", courseId);
  if (pendingOnly) params.set("pendingOnly", "true");
  const query = params.toString();
  return request(`/homework${query ? `?${query}` : ""}`);
};

export const createHomework = (homework) =>
  request("/homework", jsonRequest("POST", homework));

export const submitHomework = (homeworkId, { text, links, files }) => {
  const form = new FormData();
  if (text) form.append("Text", text);
  form.append("LinksJson", JSON.stringify(links));
  files.forEach((file) => form.append("Files", file));
  return request(`/homework/${homeworkId}/submissions`, {
    method: "POST",
    body: form,
  });
};

export const gradeSubmission = (submissionId, grade) =>
  request(
    `/homework/submissions/${submissionId}/grade`,
    jsonRequest("POST", { grade: Number(grade) }),
  );

export const downloadHomeworkAttachment = (attachmentId) =>
  requestBlob(`/homework/attachments/${attachmentId}`);

export default API_BASE_URL;
