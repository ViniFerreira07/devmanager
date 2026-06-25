import * as React from "react"
import * as ToastPrimitive from "@radix-ui/react-toast"

export type ToastProps = React.ComponentPropsWithoutRef<typeof ToastPrimitive.Root>

export const ToastProvider = ToastPrimitive.Provider
export const ToastViewport = React.forwardRef<
  React.ElementRef<typeof ToastPrimitive.Viewport>,
  React.ComponentPropsWithoutRef<typeof ToastPrimitive.Viewport>
>(({ className, ...props }, ref) => (
  <ToastPrimitive.Viewport
    ref={ref}
    className={`fixed top-4 right-4 z-50 flex flex-col gap-2 w-96 max-w-[100vw] ${className || ""}`}
    {...props}
  />
))

export const Toast = React.forwardRef<
  React.ElementRef<typeof ToastPrimitive.Root>,
  ToastProps
>(({ className, ...props }, ref) => (
  <ToastPrimitive.Root
    ref={ref}
    className={`group pointer-events-auto relative flex w-full items-center justify-between space-x-4 rounded-md border border-border bg-card px-4 py-3 text-sm shadow-lg data-[swipe=cancel]:translate-x-0 data-[swipe=move]:translate-x-[var(--radix-toast-swipe-move-x)] data-[swipe=move]:transition-[transform_200ms_ease-out] ${className || ""}`}
    {...props}
  />
))

export const ToastAction = ToastPrimitive.Action
export const ToastClose = ToastPrimitive.Close
export const ToastTitle = ToastPrimitive.Title
export const ToastDescription = ToastPrimitive.Description
export const ToastLabel = ToastPrimitive.Label