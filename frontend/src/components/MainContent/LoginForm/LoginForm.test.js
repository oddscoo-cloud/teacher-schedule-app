import { fireEvent, render, screen, waitFor } from "@testing-library/react";
jest.mock("react-router-dom", () => ({ useNavigate: () => jest.fn() }), { virtual: true });
import LoginForm from "./LoginForm";

describe("teacher registration", () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  test("submits teacher details and confirms successful registration", async () => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ success: true, message: "Registration successful" }),
    });

    render(<LoginForm />);

    fireEvent.click(screen.getByRole("button", { name: "Зарегистрироваться" }));
    fireEvent.change(screen.getByLabelText("Имя"), { target: { value: "Anna" } });
    fireEvent.change(screen.getByLabelText("Фамилия"), { target: { value: "Smith" } });
    fireEvent.change(screen.getByLabelText("Email"), { target: { value: "anna@example.test" } });
    fireEvent.change(screen.getByLabelText("Пароль"), { target: { value: "secure-password" } });
    fireEvent.click(screen.getByRole("button", { name: "Зарегистрироваться" }));

    await waitFor(() => expect(global.fetch).toHaveBeenCalledTimes(1));
    const [url, options] = global.fetch.mock.calls[0];
    expect(url).toContain("/auth/register/teacher");
    expect(JSON.parse(options.body)).toEqual({
      email: "anna@example.test",
      password: "secure-password",
      firstName: "Anna",
      lastName: "Smith",
      userType: "standalone-teacher",
    });
    expect(await screen.findByRole("status")).toHaveTextContent("Аккаунт преподавателя создан");
  });
});
