window.electroSession = {
  async login(usuario, password) {
    const response = await fetch('/auth/login', {
      method: 'POST', credentials: 'same-origin',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ usuario, password })
    });
    return response.ok ? (await response.json()).destino : null;
  },
  async logout() {
    await fetch('/auth/logout', { method: 'POST', credentials: 'same-origin' });
  },
  startActivity() {
    if (this.started) return;
    this.started = true;
    let lastPing = 0;
    const ping = async () => {
      const now = Date.now();
      if (now - lastPing < 5 * 60 * 1000) return;
      lastPing = now;
      try {
        const response = await fetch('/auth/activity', { method: 'POST', credentials: 'same-origin' });
        if (response.status === 401) window.location.replace('/login');
      } catch { /* The next interaction can retry after connectivity returns. */ }
    };
    for (const event of ['pointerdown', 'keydown', 'scroll']) document.addEventListener(event, ping, { passive: true });
  }
};
