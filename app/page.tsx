const modules = [
  {
    title: "Students",
    text: "Searchable student directory with department filters, pagination, and full create, edit, and delete workflows.",
  },
  {
    title: "Departments",
    text: "Organize the school into academic departments and keep every record attached to the right one.",
  },
  {
    title: "Courses",
    text: "Manage the course catalogue with credit hours and quick links into enrollments and attendance.",
  },
  {
    title: "Instructors",
    text: "Track teaching staff, hire dates, and office assignments across buildings and rooms.",
  },
  {
    title: "Enrollments",
    text: "Connect students to courses, record grades, and browse the full enrollment history with filters.",
  },
  {
    title: "Attendance",
    text: "Daily class attendance with present and absent tracking, summaries, and course and date filters.",
  },
]

const stats = [
  { value: "92", label: "Students" },
  { value: "20", label: "Instructors" },
  { value: "20", label: "Courses" },
  { value: "8", label: "Departments" },
  { value: "1,365", label: "Attendance records" },
]

export default function HomePage() {
  return (
    <main className="min-h-screen bg-[#F6F4EE] text-[#1C2A24]">
      <header className="mx-auto flex w-full max-w-6xl items-center justify-between px-6 py-5">
        <div className="flex items-center gap-3">
          <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-[#1E5B44] font-serif text-xl font-bold text-[#F6F4EE]">
            S
          </span>
          <span className="text-lg font-semibold tracking-tight">
            STDMS <span className="font-normal text-[#55677F]">School Manager</span>
          </span>
        </div>
        <a
          href="https://github.com/Omar-Mohamed-Fattouh/STDMS"
          target="_blank"
          rel="noreferrer"
          className="rounded-lg bg-[#152A21] px-4 py-2 text-sm font-medium text-white transition hover:bg-[#1E5B44]"
        >
          View on GitHub
        </a>
      </header>

      <section className="mx-auto w-full max-w-6xl px-6 pt-10 pb-14 text-center sm:pt-16">
        <p className="text-xs font-semibold uppercase tracking-[0.25em] text-[#9A6B2F]">
          Student &amp; Teacher Data Management
        </p>
        <h1 className="mx-auto mt-5 max-w-3xl font-serif text-4xl font-bold leading-tight tracking-tight text-[#152A21] sm:text-6xl">
          Your school, beautifully organized.
        </h1>
        <p className="mx-auto mt-5 max-w-2xl text-base leading-relaxed text-[#43536B] sm:text-lg">
          STDMS is a complete school records system: students, teachers, courses,
          enrollments, and attendance — all in one modern, role-based workspace
          built with ASP.NET Core 8 and SQL Server.
        </p>
        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <a
            href="https://github.com/Omar-Mohamed-Fattouh/STDMS"
            target="_blank"
            rel="noreferrer"
            className="rounded-lg bg-[#1E5B44] px-6 py-3 text-sm font-semibold text-white transition hover:bg-[#174734]"
          >
            Browse the source code
          </a>
          <a
            href="#modules"
            className="rounded-lg border border-[#1E5B44] px-6 py-3 text-sm font-semibold text-[#1E5B44] transition hover:bg-[#1E5B44] hover:text-white"
          >
            Explore the modules
          </a>
        </div>

        <dl className="mx-auto mt-14 grid max-w-4xl grid-cols-2 gap-4 sm:grid-cols-5">
          {stats.map((s) => (
            <div key={s.label} className="rounded-xl border border-[#E3DECF] bg-white px-4 py-5">
              <dt className="order-2 mt-1 block text-xs font-medium uppercase tracking-wide text-[#55677F]">
                {s.label}
              </dt>
              <dd className="font-serif text-3xl font-bold text-[#1E5B44]">{s.value}</dd>
            </div>
          ))}
        </dl>
      </section>

      <section id="modules" className="mx-auto w-full max-w-6xl px-6 pb-16">
        <h2 className="text-center font-serif text-3xl font-bold text-[#152A21]">
          Everything a school office runs on
        </h2>
        <p className="mx-auto mt-3 max-w-xl text-center text-[#43536B]">
          Six connected modules with role-based access for administrators and
          staff, verified end to end with live data.
        </p>
        <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {modules.map((m) => (
            <article key={m.title} className="rounded-xl border border-[#E3DECF] bg-white p-6">
              <h3 className="font-serif text-xl font-semibold text-[#152A21]">{m.title}</h3>
              <p className="mt-2 text-sm leading-relaxed text-[#43536B]">{m.text}</p>
            </article>
          ))}
        </div>

        <div className="mt-12 rounded-xl bg-[#152A21] px-6 py-8 text-center sm:px-10">
          <h2 className="font-serif text-2xl font-bold text-white">About this page</h2>
          <p className="mx-auto mt-3 max-w-2xl text-sm leading-relaxed text-[#C9D4CC] sm:text-base">
            The full STDMS application is an ASP.NET Core 8 MVC system backed by
            SQL Server, with Identity authentication, self-hosted assets, and a
            perfect 100 across Lighthouse performance, accessibility, best
            practices, and SEO on every page. This showcase page is published
            with the project; the complete source code, documentation, and run
            instructions live in the GitHub repository.
          </p>
          <a
            href="https://github.com/Omar-Mohamed-Fattouh/STDMS"
            target="_blank"
            rel="noreferrer"
            className="mt-6 inline-block rounded-lg bg-[#9A6B2F] px-6 py-3 text-sm font-semibold text-white transition hover:bg-[#7C5518]"
          >
            Open the repository
          </a>
        </div>
      </section>

      <footer className="border-t border-[#E3DECF] py-6 text-center text-sm text-[#55677F]">
        STDMS — Student &amp; Teacher Data Management System
      </footer>
    </main>
  )
}
