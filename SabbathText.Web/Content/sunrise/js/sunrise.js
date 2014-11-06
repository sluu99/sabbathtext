
$(function() {

    /*
        Subscription form
    =================================================================*/

    var messages = config.subscription_form_tooltips,
        error = false,
        $form = $('#subscription-form'),
        $email = $('#subscription-email'),
        $button = $('#subscription-submit'),
        $tooltip;

    function renderTooltip(type, message) {
        if (!$tooltip) {
            $tooltip = $('<p id="subscription-tooltip" class="subscription-tooltip"></p>').appendTo($form).hide();
        }

        if (type == 'success') {
            $tooltip.removeClass('error').addClass('success');
        } else {
            $tooltip.removeClass('success').addClass('error');
        }

        $tooltip.text(message).fadeIn(200);
    }

    function hideTooltip() {
        if ($tooltip) {
            $tooltip.fadeOut(200);
        }
    }

    function changeFormState(type, message) {
        $email.removeClass('success error');

        if (type == 'normal') {
            hideTooltip();
        } else {
            renderTooltip(type, message);
            $email.addClass(type);
        }
    }

    $form.submit(function(event) {
        event.preventDefault();

        var email = $email.val();

        if (email.length == 0) {
            changeFormState('error', messages['empty_email']);
        } else {
            $.post('subscribe.php', {
                'email': email,
                'ajax': 1
            }, function(data) {
                if (data.status == 'success') {
                    changeFormState('success', messages['success']);
                } else {
                    switch (data.error) {
                        case 'empty_email':
                        case 'invalid_email':
                        case 'already_subscribed':
                            changeFormState('error', messages[data.error]);
                            break;

                        default:
                            changeFormState('error', messages['default_error'])
                            break;
                    }
                }
            }, 'json');
        }
    });


    // Remove tooltip on text change

    $email.on('change focus click keydown', function() {
        if($email.val().length > 0) {
            changeFormState('normal');
        }
    });
});