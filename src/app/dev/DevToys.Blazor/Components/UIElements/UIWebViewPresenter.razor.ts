export function webViewNavigateToUri(iframe: HTMLIFrameElement, uri: string): void {
    iframe.src = uri;
}

export function webViewNavigateToHtml(iframe: HTMLIFrameElement, html: string): void {
    const iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
    iframeDocument.body.innerHTML = html;
}