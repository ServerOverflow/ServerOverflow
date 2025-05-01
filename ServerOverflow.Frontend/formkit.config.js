import { generateClasses } from '@formkit/themes'
import { defineFormKitConfig } from '@formkit/vue'
import { en } from '@formkit/i18n'

const textClassification = {
    input: `
  $reset
   input 
   input-bordered
   w-full 
   max-w-xs
   focus-within:input-info
   formkit-invalid:input-error
  `,
    label: 'font-bold text-sm formkit-invalid:text-red-500'
}

const buttonClassification = {
    input: '$reset btn btn-info'
}

const theme = {
    global: {
        help: 'text-xs text-gray-500 m-1',
        message: '$reset text-error text-xs m-1',
        label: '$reset label-text m-1',
        outer: '$reset my-2'
    },
    button: buttonClassification,
    date: textClassification,
    'datetime-local': textClassification,
    checkbox: {
        input: '$reset checkbox checkbox-accent',
        inner: '$reset inline',
        label: '$reset ml-2 label-text',
        legend: '$reset font-bold px-1',
        fieldset: '$reset card card-bordered border-accent p-2',
        wrapper: '$reset cursor-pointer flex items-center justify-start max-w-fit'
    },
    email: textClassification,
    month: textClassification,
    number: textClassification,
    password: textClassification,
    radio: {
        input: '$reset radio radio-accent',
        inner: '$reset inline',
        label: '$reset ml-2 label-text',
        legend: '$reset font-bold px-1',
        fieldset: '$reset card card-bordered border-accent p-2',
        wrapper: '$reset cursor-pointer flex items-center justify-start max-w-fit'
    },
    range: {
        input: '$reset range range-secondary'
    },
    search: textClassification,
    select: textClassification,
    submit: buttonClassification,
    tel: textClassification,
    text: textClassification,
    textarea: {
        input: `
    $reset
    textarea
    input-bordered
    focus-within:input-info
    formkit-invalid:input-error
    `
    },
    time: textClassification,
    url: textClassification,
    week: textClassification
}

export default defineFormKitConfig({
    locales: { en },
    locale: 'en',
    config: {
        classes: generateClasses(theme)
    }
})