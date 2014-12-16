<?php $this->load->view('common/header'); ?>
<div class="container">
    <div class="text-center">
        <div class="col-md-2 col-md-offset-2">
            <h1>ユーザ</h1>
            <ul>
                <li class="top">
                    <a href="<?php echo base_url() ?>index.php/user">一覧</a>
                </li>
            </ul>
        </div>
        <div class="col-md-3 col-md-offset-3">
            <h1>BOX</h1>
            <ul>
                <li class="top">
                    <a href="<?php echo base_url() ?>index.php/box">一覧</a>
                </li>
                <li class="top">
                    <a href="<?php echo base_url() ?>index.php/box/register">登録</a>
                </li>
            </ul>
        </div>
        <div class="col-md-2 col-md-offset-2">
            <h1>音楽</h1>
            <ul>
                <li class="top">
                    <a href="<?php echo base_url() ?>index.php/music">一覧</a>
                </li>
                <li class="top">
                    <a href="<?php echo base_url() ?>index.php/music/register">登録</a>
                </li>
                <li class="top">
                    <a href="<?php echo base_url() ?>index.php/statistic">ランキング</a>
                </li>
            </ul>
        </div>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>
