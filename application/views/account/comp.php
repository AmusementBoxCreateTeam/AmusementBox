<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <ul class="breadcrumb">
        <li>アカウント情報変更<span class="divider"></span></li>
        <li>アカウント情報変更確認<span class="divider"></span></li>
        <li class="active">アカウント情報変更完了</li>
    </ul>
    <h2>アカウント情報変更完了</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">アカウント情報変更完了</div>
        </div>
        <p class="text-center">
            アカウント情報変更が完了しました。<br />
            <a href="<?php echo base_url() . 'index.php/account' ?>">アカウント情報変更へ</a>
        </p>
    </div>
</div>
<?php
$this->load->view('common/footer');
