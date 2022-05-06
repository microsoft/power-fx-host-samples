
function getServerUrl() {
    return `${window.location.origin}${window.location.pathname}`;
}

export async function sendDataAsync(endpoint: string, data: string): Promise<Response> {
    const url = getServerUrl();
    return await fetch(url + endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: data
    });
}