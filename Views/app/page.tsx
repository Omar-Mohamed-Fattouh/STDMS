"use client"

import { ThemeProvider } from "../components/theme-provider"

export default function SyntheticV0PageForDeployment() {
  return (
    <ThemeProvider>
      <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
        <div className="text-center">
          <p className="text-sm uppercase tracking-[0.2em] text-muted-foreground">
            STDMS
          </p>
          <h1 className="mt-4 text-4xl font-bold tracking-tight">Next.js app is ready</h1>
          <p className="mt-3 text-lg text-muted-foreground">
            The placeholder deployment page has been restored to a valid client-rendered entry point.
          </p>
        </div>
      </main>
    </ThemeProvider>
  )
}
