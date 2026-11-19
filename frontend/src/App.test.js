import { render, screen } from "@testing-library/react";
import CourseFilter from "./components/Students/CourseFilter";

test("shows all available courses in the course filter", () => {
  render(
    <CourseFilter
      courses={[{ id: 3, name: "Mathematics" }]}
      value=""
      onChange={() => {}}
    />
  );

  expect(screen.getByRole("option", { name: "Mathematics" })).toBeInTheDocument();
});
