async function pollForToken(deviceCode, interval) {
    return new Promise((resolve, reject) => {
        const intervalMs = interval * 1000;

        const poll = () => {
            $.get(`/user/poll/${deviceCode}`)
                .done(response => {
                    if (!response.success) {
                        setTimeout(poll, intervalMs);
                        return;
                    }
                    
                    resolve(response.status);
                })
                .fail(err => {
                    reject(err.responseJSON.status);
                });
        };

        poll();
    });
}

$(document).ready(function() {
    $("#addAccount").click(async function () {
        if (window.msaURL) {
            window.open(window.msaURL);
            return;
        }
        
        let button = $(this);
        let help = button.parent().find(".help");
        help.show();
        help.text("Loading, please wait...")
        button.addClass("is-disabled");
        button.attr("disabled", "true");
        let resp = await $.get("/user/devicecode");
        help.text("Click the button again and confirm");
        button.removeClass("is-disabled");
        button.removeAttr("disabled");
        window.msaURL = `${resp.verification_uri}?otc=${resp.user_code}`;
        pollForToken(resp.device_code, resp.interval)
            .then(response => {
                help.text(response);
                window.msaURL = undefined;
                location.href = location.href;
            })
            .catch(error => {
                help.text(error);
                window.msaURL = undefined;
            });
    });
});
