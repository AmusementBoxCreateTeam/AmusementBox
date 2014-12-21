<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <h2>アカウント情報変更</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">メールアドレス変更</div>
        </div>
        <?php
        $url = 'account/conf/';
        $url .=!empty($id) ? $id : '';
        echo form_open($url);
        ?>
        <div class="panel-body">
            <input type="checkbox" id="check_mail" name="check_mail" value="1" <?php echo set_checkbox('check_mail', '1'); ?> />メールアドレスの変更
                <table class="table table-bordered">
                    <tr class="account_mail">
                        <th>メールアドレス&nbsp;<span class="btn-danger">必須</span></th>
                        <td><input type="text" name="mail" placeholder="メールアドレス" value="<?php echo set_value('mail') ?>"/></td>
                    </tr>
                    <tr class="account_mail">
                        <th>メールアドレス（確認）&nbsp;<span class="btn-danger">必須</span></th>
                        <td><input type="text" name="mail_conf" placeholder="メールアドレス（確認）" value="<?php echo set_value('mail_conf') ?>" /></td>
                    </tr>
                </table>
        </div>
        <div class="panel-body">
            <input type="checkbox" id="check_pass" name="check_pass"  value="1" <?php echo set_checkbox('check_pass', '1'); ?> />パスワードの変更
                <table class="table table-bordered">
                    <tr class="account_pass">
                        <th>パスワード&nbsp;<span class="btn-danger">必須</span></th>
                        <td><input type="password" name="pass" placeholder="パスワード" value="<?php echo set_value('pass') ?>"/></td>
                    </tr>
                    <tr class="account_pass">
                        <th>パスワード（確認）&nbsp;<span class="btn-danger">必須</span></th>
                        <td><input type="password" name="pass_conf" placeholder="パスワード（確認）" value="<?php echo set_value('pass_conf') ?>" /></td>
                    </tr>
                </table>
        </div>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-success btn-lg">登録</button></div>
        </form>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>
