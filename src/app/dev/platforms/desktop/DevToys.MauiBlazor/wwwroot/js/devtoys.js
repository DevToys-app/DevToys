//var _dotNetObjRef;

//window.blazorExtensions = {


//    toggleResizeTracking: function (dotNetObjRef) {
//        _dotNetObjRef = dotNetObjRef;

//        dotNetObjRef.invokeMethodAsync('ToggleResizeTracking').then(isResizeTrackingEnabled => {
//            if (isResizeTrackingEnabled) {
//                blazorExtensions.enableResizeTracking();
//            } else {
//                blazorExtensions.disableResizeTracking();
//            }
//        });
//    },
//    enableResizeTracking: function () {
//        console.log('enableResizeTracking invoked');
//        blazorExtensions.handleWindowResize();
//        window.addEventListener("resize", blazorExtensions.handleWindowResize);
//    },
//    disableResizeTracking: function () {
//        console.log('disableResizeTracking invoked');
//        window.removeEventListener("resize", blazorExtensions.handleWindowResize);
//    },
//    handleWindowResize: function () {
//        let currentState = window.getComputedStyle(document.body, ':before').content.replace(/\"/g, '');
//        _dotNetObjRef.invokeMethodAsync('OnWindowResize', currentState);
//        /*
//        _dotNetObjRef.invokeMethodAsync('IsNearWindowBottom', content.scrollTop(), content.height(), container.height())
//            .then(isNearWindowBottom => {
//                console.log('isNearWindowBottom: ' + isNearWindowBottom);
//                if (isNearWindowBottom) {
//                    alert("You are NEAR the bottom!");
//                }
//            });

//        _dotNetObjRef.invokeMethodAsync('IsAtWindowBottom', content.scrollTop(), content.height(), container.height())
//            .then(isAtWindowBottom => {
//                console.log('isAtWindowBottom: ' + isAtWindowBottom);
//                if (isAtWindowBottom) {
//                    alert("You are AT the bottom!");
//                }
//            });
//*/
//    }
//};
