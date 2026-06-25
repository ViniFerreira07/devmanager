import { FormProvider, useForm, UseFormProps, FieldValues } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import * as React from "react"

interface FormProps<T extends FieldValues> extends Omit<UseFormProps<T>, "resolver"> {
  schema: z.ZodSchema<T>
  onSubmit: (data: T) => void
  children: React.ReactNode
}

export function Form<T extends FieldValues>({ schema, onSubmit, children, ...props }: FormProps<T>) {
  const methods = useForm<T>({ resolver: zodResolver(schema), ...props })
  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(onSubmit)}>{children}</form>
    </FormProvider>
  )
}

export { FormProvider, useFormContext } from "react-hook-form"