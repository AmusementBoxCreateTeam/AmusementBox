<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <h2>アカウント情報変更</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">メールアドレス変更</div>
        </div>
        <?php
        $url = 'account/comp/';
        echo form_open($url);
        ?>
        <div class="panel-body">
            <table class="table table-bordered">
                <?php if (!empty($post['check_mail'])) { ?>
                    <tr>
                        <th>メールアドレス&nbsp;</th>
                        <td><?php echo set_value('mail') ?></td>
                    </tr>
                <?php } ?>
                <?php if (!empty($post['check_pass'])) { ?>
                    <tr>
                        <th>パスワード&nbsp;</th>
                        <td>(セキュリティ上のため表示していません)</td>
                    </tr>
                <?php } ?>
            </table>
        </div>
        <?php
        foreach ($post as $key => $val) {
            echo form_hidden($key, $val);
        }
        ?>
        <div class="text-center" style="margin-bottom:10px;"><button type="button" onclick="history.back()" class="btn btn-info btn-lg" style="margin: 30px;">戻る</button><button type="submit" class="btn btn-success btn-lg">登録</button></div>
        </form>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>
