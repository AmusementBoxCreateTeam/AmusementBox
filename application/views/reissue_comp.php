<?php echo $this->load->view('login/header'); ?>
<div class="container">
    <h2 class="text-center">送信完了</h2>
    <div class="text-center">
        <?php echo!empty($error) ? $error : 'メールが送信されました。<br />'; ?>
        <a href="<?php echo base_url()."index.php/index/login" ?>">&lt;&lt;&nbsp;ログイン画面に戻る</a>
    </div>
</div>
<?php
echo $this->load->view('login/footer');
