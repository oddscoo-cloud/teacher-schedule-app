import { fetchStudents, createStudent } from "./api";

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
});
