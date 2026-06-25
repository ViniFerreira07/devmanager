'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

export default function HomePage() {
  const router = useRouter();
  const [checked, setChecked] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('devmanager-token');
    if (token) {
      router.replace('/dashboard');
    } else {
      router.replace('/login');
    }
    setChecked(true);
  }, [router]);

  if (!checked) {
    return null;
  }

  return null;
}
