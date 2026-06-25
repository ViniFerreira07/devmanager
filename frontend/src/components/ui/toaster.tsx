import { Toaster as SonnerToaster } from "sonner"
import * as React from "react"

export const Toaster = SonnerToaster

export function toast(message: string) {
  const event = new CustomEvent("toast", { detail: { message } })
  document.dispatchEvent(event)
}