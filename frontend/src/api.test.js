import {
  createStudent,
  fetchLessons,
  fetchHomeworks,
  fetchStudents,
  submitHomework,
} from "./api";

describe("student API client", () => {
  beforeEach(() => {
    localStorage.setItem("token", "test-token");
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  test("fetchStudents encodes filters and sends the bearer token", async () => {
    global.fetch.mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => [],
    });

    await fetchStudents({ search: "Anna Smith", course: 12, status: "Active" });

    const [url, options] = global.fetch.mock.calls[0];
    expect(url).toContain("/students?search=Anna+Smith&course=12&status=Active");
    expect(options.headers.Authorization).toBe("Bearer test-token");
  });

  test("createStudent surfaces API validation errors", async () => {
    global.fetch.mockResolvedValue({
      ok: false,
      status: 400,
      text: async () => JSON.stringify({ message: "Invalid student" }),
    });

    await expect(createStudent({ firstName: "" })).rejects.toThrow("Invalid student");
  });

  test("fetchHomeworks sends student and course filters", async () => {
    global.fetch.mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => [],
    });

    await fetchHomeworks({ studentId: 7, courseId: 3 });

    expect(global.fetch.mock.calls[0][0]).toContain("/homework?studentId=7&courseId=3");
  });

  test("fetchLessons encodes date range and optional workspace filters", async () => {
    global.fetch.mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => [],
    });

    await fetchLessons({
      from: "2026-03-01T00:00:00.000Z",
      to: "2026-03-08T00:00:00.000Z",
      studentId: 7,
      courseId: 3,
      status: "Scheduled",
    });

    const [url, options] = global.fetch.mock.calls[0];
    expect(url).toContain("/lessons?from=2026-03-01T00%3A00%3A00.000Z");
    expect(url).toContain("&to=2026-03-08T00%3A00%3A00.000Z");
    expect(url).toContain("&studentId=7&courseId=3&status=Scheduled");
    expect(options.headers.Authorization).toBe("Bearer test-token");
  });

  test("submitHomework lets the browser set the multipart content type", async () => {
    global.fetch.mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({}),
    });
    const image = new File(["photo"], "photo.png", { type: "image/png" });

    await submitHomework(5, { text: "answer", links: ["https://example.test"], files: [image] });

    const [, options] = global.fetch.mock.calls[0];
    expect(options.body).toBeInstanceOf(FormData);
    expect(options.headers["Content-Type"]).toBeUndefined();
    expect(options.headers.Authorization).toBe("Bearer test-token");
  });
});
