import Swal from 'sweetalert2';
import 'sweetalert2/dist/sweetalert2.min.css';

interface ConfirmOptions {
  title: string;
  text?: string;
  icon?: 'warning' | 'error' | 'success' | 'info' | 'question';
  confirmButtonText?: string;
  cancelButtonText?: string;
  variant?: 'destructive' | 'primary';
}

export const confirmAlert = async ({
  title,
  text,
  icon = 'warning',
  confirmButtonText = 'Confirmar',
  cancelButtonText = 'Cancelar',
  variant = 'primary',
}: ConfirmOptions) => {
  const isDark = typeof document !== 'undefined' && document.documentElement.classList.contains('dark');

  const confirmBtnClass = variant === 'destructive'
    ? 'px-4 py-2 text-sm font-medium rounded-md bg-red-600 text-white hover:bg-red-700 transition-colors focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 cursor-pointer'
    : 'px-4 py-2 text-sm font-medium rounded-md bg-blue-600 text-white hover:bg-blue-700 transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 cursor-pointer';

  const cancelBtnClass = 'px-4 py-2 text-sm font-medium rounded-md border border-gray-300 dark:border-zinc-700 bg-transparent hover:bg-gray-100 dark:hover:bg-zinc-800 transition-colors focus:outline-none focus:ring-2 focus:ring-zinc-500 focus:ring-offset-2 cursor-pointer ml-3';

  return Swal.fire({
    title,
    text,
    icon,
    showCancelButton: true,
    confirmButtonText,
    cancelButtonText,
    background: isDark ? '#09090b' : '#ffffff',
    color: isDark ? '#fafafa' : '#09090b',
    customClass: {
      popup: 'rounded-xl border border-zinc-200 dark:border-zinc-800 shadow-lg font-sans',
      title: 'text-lg font-semibold tracking-tight',
      htmlContainer: 'text-sm text-zinc-500 dark:text-zinc-400',
      confirmButton: confirmBtnClass,
      cancelButton: cancelBtnClass,
      actions: 'flex gap-2 justify-end w-full px-6 pb-2',
    },
    buttonsStyling: false,
  });
};
