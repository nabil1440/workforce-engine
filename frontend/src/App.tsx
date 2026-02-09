function App() {
  return (
    <main className='min-h-screen bg-slate-50'>
      <section className='mx-auto flex max-w-5xl flex-col gap-10 px-6 py-16'>
        <header className='flex flex-col gap-4'>
          <p className='text-sm font-semibold uppercase tracking-[0.3em] text-slate-500'>
            Workforce Dashboard
          </p>
          <h1 className='text-4xl font-semibold text-slate-900 md:text-5xl'>
            Vite + React + Tailwind starter
          </h1>
          <p className='max-w-2xl text-base text-slate-600 md:text-lg'>
            This frontend is ready for UI work. Tailwind is wired, TypeScript is
            on, and you can start building screens immediately.
          </p>
        </header>

        <div className='grid gap-4 md:grid-cols-3'>
          {[
            {
              title: 'Fast builds',
              description:
                'Vite handles instant dev and optimized production builds.'
            },
            {
              title: 'Typed UI',
              description:
                'React + TypeScript ensures safe, predictable components.'
            },
            {
              title: 'Tailwind ready',
              description: 'Utility classes are available for rapid styling.'
            }
          ].map(card => (
            <div
              key={card.title}
              className='rounded-2xl border border-slate-200 bg-white p-5 shadow-sm'
            >
              <h2 className='text-lg font-semibold text-slate-900'>
                {card.title}
              </h2>
              <p className='mt-2 text-sm text-slate-600'>{card.description}</p>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}

export default App;
