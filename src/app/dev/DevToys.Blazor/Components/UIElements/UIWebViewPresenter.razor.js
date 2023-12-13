export function webViewNavigateToUri(iframe, uri) {
    iframe.src = uri;
}
export function webViewNavigateToHtml(iframe, html) {
    const iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
    iframeDocument.body.innerHTML = html;
}
//# sourceMappingURL=UIWebViewPresenter.razor.js.map