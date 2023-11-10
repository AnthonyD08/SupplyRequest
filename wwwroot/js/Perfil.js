$('#AbrirModalCambiarPassword').click(function() {
    $('#ModalCambiarPassword').modal('show');
});

$('#btnCambiarPassword').click(function() {
    if (validarCamposPassword()) {

        var passwordActual = document.getElementById('InputPasswordActual').value;

        var passwordNuevo = document.getElementById('InputNuevoPassword').value;
        var passwordConfirmar = document.getElementById('InputConfirmarPassword').value;

        if (passwordNuevo != passwordConfirmar) {
            $('#modalVentanaFallida').find('.modal-body p').text('Verifique que las claves coincidan');
            $('#modalVentanaFallida').modal('show');
            return;
        }

        if (passwordNuevo == passwordActual) {
            $('#modalVentanaFallida').find('.modal-body p').text('La nueva clave no puede ser igual a la actual');
            $('#modalVentanaFallida').modal('show');
            return;
        }

        var b64OldPassword = btoa(passwordActual);
        var b64NewPassword = btoa(passwordNuevo);

        $.ajax({
            url: "/User/Password?oldPassword=" + b64OldPassword + "&newPassword=" + b64NewPassword,
            type: "POST",
            success: function (response) {
                $('#modalVentanaExitosa').find('.modal-body p').text('Su clave ha sido actualizada exitosamente');
                $('#modalVentanaExitosa').modal('show');
            },
            error: function (request, status, err) {
                $('#modalVentanaFallida').find('.modal-body p').text(request.responseText);
                $('#modalVentanaFallida').modal('show');
            }
        });
    }
});

$("#btnMensajeExitoso").click(function () {
    $('#modalVentanaExitosa').modal('hide');
    location.reload();
});

$("#btnMensajeFallido").click(function () {
    $('#modalVentanaFallida').modal('hide');
    location.reload();
});

$('#btnCerrarCambiarPassword').click(function () {
    $('#ModalCambiarPassword').modal('hide');
});

function validarCamposPassword() {
    var bandera = true;
    var passwordActual = document.getElementById('InputPasswordActual').value;
    var passwordNuevo = document.getElementById('InputNuevoPassword').value;
    var passwordConfirmar = document.getElementById('InputConfirmarPassword').value;

    if (passwordActual == "") {
        $("#InputPasswordActual").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#InputPasswordActual').css('border', '1px solid #ced4da');
    }
    if (passwordNuevo == "") {
        $("#InputNuevoPassword").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#InputNuevoPassword').css('border', '1px solid #ced4da');
    }
    if (passwordConfirmar == "") {
        $("#InputConfirmarPassword").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#InputConfirmarPassword').css('border', '1px solid #ced4da');
    }

    return bandera;

}